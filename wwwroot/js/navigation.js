document.addEventListener("DOMContentLoaded", function () {
  const hamburger = document.querySelector(".hamburger");
  const mobileMenu = document.querySelector(".mobile-menu");

  if (hamburger && mobileMenu) {
    hamburger.addEventListener("click", function (e) {
      e.stopPropagation();
      hamburger.classList.toggle("active");
      mobileMenu.classList.toggle("active");
    });

    const mobileLinks = mobileMenu.querySelectorAll(".red-button");
    mobileLinks.forEach((link) => {
      link.addEventListener("click", function () {
        hamburger.classList.remove("active");
        mobileMenu.classList.remove("active");
      });
    });

    document.addEventListener("click", function (event) {
      if (
        !hamburger.contains(event.target) &&
        !mobileMenu.contains(event.target)
      ) {
        hamburger.classList.remove("active");
        mobileMenu.classList.remove("active");
      }
    });
  }

  checkAuthAndUpdateNav();
});

async function checkAuthAndUpdateNav() {
  try {
    const response = await fetch("/Account/CurrentUser");
    const data = await response.json();

    if (data.success && data.isAuthenticated) {
      updateNavForLoggedInUser(data.username, data.role);
    } else {
      updateNavForGuest();
    }
  } catch (error) {
    console.error("Błąd sprawdzania autoryzacji:", error);
    updateNavForGuest();
  }
}

function updateNavForLoggedInUser(username, role) {
  const desktopNav = document.querySelector(".nav-button-container");
  const mobileNav = document.querySelector(".mobile-menu");

  let desktopHTML = `
        <a href="/" class="red-button">Strona główna</a>
        <a href="/Advertisement/Create" class="red-button">Dodaj ogłoszenie</a>
        <a href="/Profile/Index" class="red-button">Mój profil</a>
    `;

  let mobileHTML = `
        <a href="/" class="red-button">Strona główna</a>
        <a href="/Advertisement/Create" class="red-button">Dodaj ogłoszenie</a>
        <a href="/Profile/Index" class="red-button">Mój profil</a>
    `;

  if (role === "Admin") {
    desktopHTML += `
            <a href="/Admin/Dashboard" class="red-button">Zarządzaj stroną</a>
        `;
    mobileHTML += `
            <a href="/Admin/Dashboard" class="red-button">Zarządzaj stroną</a>
        `;
  }

  desktopHTML += `
        <button class="red-button" onclick="logout()">Wyloguj</button>
    `;

  mobileHTML += `
        <button class="red-button" onclick="logout()">Wyloguj</button>
    `;

  if (desktopNav) desktopNav.innerHTML = desktopHTML;
  if (mobileNav) mobileNav.innerHTML = mobileHTML;
}

function updateNavForGuest() {
  const desktopNav = document.querySelector(".nav-button-container");
  const mobileNav = document.querySelector(".mobile-menu");

  const guestHTML = `
        <a href="/" class="red-button">Strona główna</a>
        <a href="/Account/Register" class="red-button">Zarejestruj się</a>
        <a href="/Account/Login" class="red-button">Zaloguj się</a>
    `;

  if (desktopNav) desktopNav.innerHTML = guestHTML;
  if (mobileNav) mobileNav.innerHTML = guestHTML;
}

async function logout() {
  try {
    const response = await fetch("/Account/Logout", {
      method: "POST",
    });

    if (response.ok) {
      window.location.href = "/";
    }
  } catch (error) {
    console.error("Błąd wylogowania:", error);
  }
}
