// ===== Работа с токеном =====
function getToken()    { return localStorage.getItem("token"); }
function getRole()     { return localStorage.getItem("role"); }
function getUsername() { return localStorage.getItem("username"); }

function setAuth(token, role, username) {
    localStorage.setItem("token", token);
    localStorage.setItem("role", role);
    localStorage.setItem("username", username);
}

function logout() {
    localStorage.clear();
    location.href = "index.html";
}

// ===== Заголовки для API =====
function authHeaders() {
    return getToken() ? { "Authorization": "Bearer " + getToken() } : {};
}

// ===== Навигация =====
function updateNav() {
    const nav = document.getElementById("nav-auth");
    if (!nav) return;

    if (getToken()) {
        nav.innerHTML = `
            <span class="text-light me-2">
                Привет, <b>${getUsername()}</b> (${getRole()})
            </span>
            ${getRole() === "Seller"
                ? '<a href="seller.html" class="btn btn-warning btn-sm me-1">Кабинет</a>'
                : ''}
            <button onclick="logout()" class="btn btn-outline-light btn-sm">Выйти</button>`;
    } else {
        nav.innerHTML = `
            <a href="login.html" class="btn btn-outline-light btn-sm me-1">Вход</a>
            <a href="register.html" class="btn btn-warning btn-sm">Регистрация</a>`;
    }
}