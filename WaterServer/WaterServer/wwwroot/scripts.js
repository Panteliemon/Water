function tryParseDate(str, funcIfParsed) {
  try {
    let date = new Date(str);
    funcIfParsed(date);
    return true;
  } catch (err) {
    return false;
  }
}

// Appends current query parameters to language links
function syncLanguageLinks() {
  let links = document.querySelectorAll("nav > div.languages a");
  for (let aElement of links) {
    let url = new URL(aElement.href);
    url.search = window.location.search;
    aElement.href = url.toString();
  }
}

function updateQueryParam(paramName, value) {
  let strCurrentUrl = window.location.toString();
  let newUrl = new URL(strCurrentUrl);
  if ((value === null) || (value === undefined) || (value === "")) {
    if (newUrl.searchParams.has(paramName)) {
      newUrl.searchParams.delete(paramName);
    }
  } else {
    newUrl.searchParams.set(paramName, value);
  }

  let strNewUrl = newUrl.toString();
  if (strNewUrl != strCurrentUrl) {
    window.history.pushState({ path: strNewUrl }, "", newUrl);
    syncLanguageLinks();
  }
}

document.addEventListener("DOMContentLoaded", async () => {
  let signInButton = document.querySelector(".sign-in-button .btn-enter");
  if (signInButton) {
    signInButton.addEventListener("click", () => {
      signInButton.style.display = "none";
      document.querySelector(".sign-in").style.display = "block";
    });
  }

  let signOutButton = document.querySelector(".sign-in-button .btn-exit");
  if (signOutButton) {
    signOutButton.addEventListener("click", async () => {
      signOutButton.style.display = "none";
      await fetch("/api/signout", {
        method: "POST"
      });

      location.reload();
    });
  }

  let enterButton = document.querySelector(".sign-in .btn-enter");
  enterButton.addEventListener("click", async () => {
    let signInDto = {
      userName: document.getElementById("signInUser").value,
      userPassword: document.getElementById("signInPassword").value
    };

    document.querySelector(".sign-in").style.display = "none";
    document.getElementById("signInPassword").value = "";

    let result = await fetch("/api/signin", {
      method: "POST",
      body: JSON.stringify(signInDto),
      headers: {
        "Content-type": "application/json; charset=UTF-8"
      }
    });

    if (result.ok) {
      location.reload();
    } else {
      alert(LOGIN_FAIL);
      document.querySelector(".sign-in").style.display = "block";
      document.getElementById("signInPassword").value = signInDto.userPassword;
    }
  });

  let cancelButton = document.querySelector(".sign-in .btn-cancel");
  cancelButton.addEventListener("click", () => {
    signInButton.style.display = "inline-block";
    document.querySelector(".sign-in").style.display = "none";
  });
});