// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// Параметры редактора


// Функция для установки темы на <html> и обновления localStorage и текста переключателя
function setTheme(theme) {
    // Устанавливаем атрибут data-bs-theme на <html>
    document.documentElement.setAttribute("data-bs-theme", theme);

    // Пример изменения классов для navbar (при необходимости)
    var navbar = document.getElementById("mainNavbar");
    if (navbar) {
        if (theme === "dark") {
            navbar.classList.remove("navbar-light", "bg-white");
            navbar.classList.add("navbar-dark", "bg-dark");
        } else {
            navbar.classList.remove("navbar-dark", "bg-dark");
            navbar.classList.add("navbar-light", "bg-white");
        }
    }

    // Сохраняем значение темы в localStorage
    localStorage.setItem("theme", theme);
    // Обновляем текст переключателя
    var themeText = document.getElementById("themeText");
    if (themeText) {
        themeText.textContent = theme === "dark" ? "Change Mode Dark" : "Change Mode Light";
    }
}

document.addEventListener("DOMContentLoaded", function () {
    var storedTheme = localStorage.getItem("theme") || "light";
    setTheme(storedTheme);

    var themeToggle = document.getElementById("themeToggle");
    if (themeToggle) {
        themeToggle.checked = (storedTheme === "dark");
        themeToggle.addEventListener("change", function () {
            var newTheme = this.checked ? "dark" : "light";
            setTheme(newTheme);

            var isAuthenticated = '@User.Identity.IsAuthenticated'.toLowerCase();
            console.log("IsAuthenticated value: " + isAuthenticated);

            // Если пользователь авторизован, отправляем AJAX-запрос для обновления предпочтения в базе
            if (isAuthenticated === "true") {
                    $.ajax({
                        url: '@Url.Action("UpdateThemePreference", "User")',
                        type: 'POST',
                        data: {
                            theme: newTheme,
                            __RequestVerificationToken: $('meta[name="csrf-token"]').attr('content')
                        },
                        success: function (response) {
                            console.log("Theme preference updated.");
                        },
                        error: function (xhr, status, error) {
                            console.error("Error updating theme preference: " + error);
                        }
                    });
            }
        });
    }
});