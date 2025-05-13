function tryParseDate(str, funcIfParsed) {
  try {
    let date = new Date(str);
    funcIfParsed(date);
    return true;
  } catch (err) {
    return false;
  }
}