using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaterServer.ModelSimple;
using WaterServer.Xml;
using WaterServer.Xml.Dto;

namespace WaterServer.DataAccess.Repositories;

internal class SimpleFileBasedRepository : IRepository
{
    private static readonly TimeSpan cacheLifeTime = TimeSpan.FromMinutes(2);
    private object lockObj = new();

    private string storageFilePath;
    private string usersFilePath;
    private string passwordSalt;

    private Task criticalSectionTask;

    private SModel cachedModel;
    private DateTime? cacheValidUntil;

    private RootUserDto cachedUsers;
    private DateTime? usersCacheValidUntil;

    public SimpleFileBasedRepository(IWaterConfig waterConfig)
    {
        storageFilePath = Path.Combine(waterConfig.StorageRoot, "wsdata.xml");
        usersFilePath = Path.Combine(waterConfig.StorageRoot, "wsusers.xml");
        passwordSalt = waterConfig.PasswordSalt;
    }

    public async Task<SModel> ReadAll()
    {
        SModel result = null;
        await ExecuteUnderCriticalSection(() => Task.Run(() =>
        {
            if (cacheValidUntil.HasValue)
            {
                DateTime now = DateTime.Now;
                if (now < cacheValidUntil.Value)
                {
                    result = cachedModel?.Clone();
                    return;
                }
            }

            // Fallback: honest read
            if (!File.Exists(storageFilePath))
                return;

            string modelXml = File.ReadAllText(storageFilePath, Encoding.UTF8);
            result = ModelXml.ParseRoot(modelXml);

            cacheValidUntil = DateTime.Now.Add(cacheLifeTime);
            cachedModel = result?.Clone();
        }));

        return result;
    }

    public async Task WriteAll(SModel model)
    {
        await ExecuteUnderCriticalSection(() => Task.Run(() =>
        {
            model ??= SModel.Empty();

            cacheValidUntil = DateTime.Now.Add(cacheLifeTime);
            cachedModel = model.Clone();

            string modelXml = ModelXml.RootToStr(model);
            File.WriteAllText(storageFilePath, modelXml, Encoding.UTF8);
        }));
    }

    public async Task CreateUser(string name, string password)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException();

        await ExecuteUnderCriticalSection(async () =>
        {
            RootUserDto rootDto = await ReadUsers();
            rootDto ??= RootUserDto.Empty();
            rootDto.Users ??= new List<UserDto>();

            string nameLower = name.ToLower();
            string passwordHash = SecurityUtils.GetPasswordHash(password, passwordSalt);
            UserDto existing = rootDto.Users?.FirstOrDefault(user => user.Name?.ToLower() == nameLower);
            if (existing != null)
            {
                existing.PasswordHash = passwordHash;
            }
            else
            {
                rootDto.Users.Add(new UserDto()
                {
                    Name = name,
                    PasswordHash = passwordHash
                });
            }

            await WriteUsers(rootDto);
        });
    }

    public async Task<UserVerificationResult> VerifyUser(string name, string password)
    {
        if (string.IsNullOrEmpty(name))
            return new UserVerificationResult(false, null);

        bool success = false;
        string realUserName = null;
        await ExecuteUnderCriticalSection(async () =>
        {
            RootUserDto rootDto = await ReadUsers();
            if (rootDto?.Users != null)
            {
                string nameLower = name.ToLower();
                UserDto existing = rootDto.Users?.FirstOrDefault(user => user.Name?.ToLower() == nameLower);
                if (existing != null)
                {
                    string passwordHash = SecurityUtils.GetPasswordHash(password, passwordSalt);
                    success = existing.PasswordHash == passwordHash;
                    if (success)
                    {
                        realUserName = existing.Name;
                    }
                }
            }
        });

        return new UserVerificationResult(success, realUserName);
    }

    private async Task<RootUserDto> ReadUsers()
    {
        if (usersCacheValidUntil.HasValue)
        {
            DateTime now = DateTime.Now;
            if (now < usersCacheValidUntil.Value)
            {
                return cachedUsers;
            }
        }

        // Fallback: honest read
        if (!File.Exists(usersFilePath))
            return null;

        string usersXml = await File.ReadAllTextAsync(usersFilePath, Encoding.UTF8);
        cachedUsers = ModelXml.ParseRootUser(usersXml);
        usersCacheValidUntil = DateTime.Now.Add(cacheLifeTime);

        return cachedUsers;
    }

    private async Task WriteUsers(RootUserDto dto)
    {
        usersCacheValidUntil = DateTime.Now.Add(cacheLifeTime);
        cachedUsers = dto;

        string usersXml = ModelXml.RootUserToStr(dto);
        await File.WriteAllTextAsync(usersFilePath, usersXml, Encoding.UTF8);
    }

    private async Task ExecuteUnderCriticalSection(Func<Task> funcToExecute)
    {
        TaskCompletionSource tcs = new TaskCompletionSource();
        Task taskToWait = null;

        lock (lockObj)
        {
            if (criticalSectionTask != null)
            {
                taskToWait = criticalSectionTask;
            }

            // We wait for existing task, while the next one will wait for us.
            criticalSectionTask = tcs.Task;
        }

        try
        {
            if (taskToWait != null)
                await taskToWait;

            await funcToExecute();
        }
        finally
        {
            tcs.SetResult();
        }
    }
}
