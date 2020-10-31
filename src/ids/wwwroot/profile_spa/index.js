let appelement = document.getElementById("profile_spa");

let antiCsrf = appelement.getAttribute("data-anticsrf");
let oidcLoginId = appelement.getAttribute("data-oidcLoginId");
let oidcLoginName = appelement.getAttribute("data-oidcLoginName");
if (!oidcLoginName) {
    oidcLoginName = "";
}

let oidcLogin = null;
if (oidcLoginId) {
    oidcLogin = { 
        id: oidcLoginId,
        name: oidcLoginName 
    } 
}

let flags = 
    { antiCsrf: antiCsrf
    , oidcLogin: oidcLogin
    }

let app = Elm.Main.init({ flags: flags });