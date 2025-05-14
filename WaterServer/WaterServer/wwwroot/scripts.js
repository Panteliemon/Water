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