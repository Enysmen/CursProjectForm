document.addEventListener("DOMContentLoaded", function () {
    // Функция для получения CSRF-токена из метатега
    function getCsrfToken() {
        const tokenMeta = document.querySelector('meta[name="csrf-token"]');
        return tokenMeta ? tokenMeta.getAttribute("content") : "";
    }
    const csrfToken = getCsrfToken();

    // Выделение всех чекбоксов
    const selectAllCheckbox = document.getElementById("selectAll");
    const rowCheckboxes = document.querySelectorAll(".select-row");

    selectAllCheckbox.addEventListener("change", function () {
        rowCheckboxes.forEach(checkbox => {
            checkbox.checked = this.checked;
        });
    });

    // Фильтрация строк таблицы
    const filterInput = document.getElementById("filterInput");
    const tableRows = document.querySelectorAll("#userTable tbody tr");

    filterInput.addEventListener("input", function () {
        const filterValue = this.value.toLowerCase();
        tableRows.forEach(row => {
            const name = row.cells[1].innerText.toLowerCase();
            const email = row.cells[2].innerText.toLowerCase();
            if (name.includes(filterValue) || email.includes(filterValue)) {
                row.style.display = "";
            } else {
                row.style.display = "none";
            }
        });
    });

    // Функция получения выбранных строк
    function getSelectedRows() {
        return Array.from(rowCheckboxes).filter(checkbox => checkbox.checked).map(checkbox => checkbox.closest("tr"));
    }

    // Блокировка выбранных пользователей
    document.getElementById("blockSelected").addEventListener("click", async function () {
        const selectedRows = getSelectedRows();
        const userIds = selectedRows.map(row => row.querySelector(".select-row").dataset.userId);
        const response = await fetch(`/api/admin/block`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': csrfToken
            },
            body: JSON.stringify(userIds)
        });
        if (response.redirected) {
            window.location.href = response.url;
            return;
        }
        selectedRows.forEach(row => {
            row.querySelector(".status-column").innerText = "Заблокирован";
        });
        alert(`Заблокированы пользователи: ${selectedRows.map(row => row.cells[2].innerText).join(", ")}`);
    });

    // Разблокировка выбранных пользователей
    document.getElementById("unblockSelected").addEventListener("click", async function () {
        const selectedRows = getSelectedRows();
        for (const row of selectedRows) {
            const userId = row.querySelector(".select-row").dataset.userId;
            await fetch(`/api/admin/unblock/${userId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': csrfToken
                }
            });
            row.querySelector(".status-column").innerText = "Активен";
        }
        alert(`Разблокированы пользователи: ${selectedRows.map(row => row.cells[2].innerText).join(", ")}`);
    });

    // Удаление выбранных пользователей
    document.getElementById("deleteSelected").addEventListener("click", async function () {
        const selectedRows = getSelectedRows();
        const userIds = selectedRows.map(row => row.querySelector(".select-row").dataset.userId);
        const response = await fetch(`/api/admin/delete`, {
            method: 'POST', // Используем POST для удаления
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': csrfToken
            },
            body: JSON.stringify(userIds)
        });
        if (response.redirected) {
            window.location.href = response.url;
            return;
        }
        selectedRows.forEach(row => row.remove());
        alert(`Удалены пользователи: ${selectedRows.map(row => row.cells[2].innerText).join(", ")}`);
    });



    // Добавление роли администратора для выбранных пользователей
    document.getElementById("addAdminSelected").addEventListener("click", async function () {
        const selectedRows = getSelectedRows();
        if (selectedRows.length === 0) {
            alert("Пожалуйста, выберите хотя бы одного пользователя для добавления в администраторы.");
            return;
        }
        const userIds = selectedRows.map(row => row.querySelector(".select-row").dataset.userId);
        const response = await fetch(`/api/admin/addadmin`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': csrfToken
            },
            body: JSON.stringify(userIds)
        });
        if (response.redirected) {
            window.location.href = response.url;
            return;
        }
        alert("Пользователи добавлены в администраторы.");
        window.location.reload();
    });

    // Удаление роли администратора для выбранных пользователей
    document.getElementById("removeAdminSelected").addEventListener("click", async function () {
        const selectedRows = getSelectedRows();
        if (selectedRows.length === 0) {
            alert("Пожалуйста, выберите хотя бы одного пользователя для удаления из администраторов.");
            return;
        }
        const userIds = selectedRows.map(row => row.querySelector(".select-row").dataset.userId);
        const response = await fetch(`/api/admin/removeadmin`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': csrfToken
            },
            body: JSON.stringify(userIds)
        });
        if (response.redirected) {
            window.location.href = response.url;
            return;
        }
        alert("Роль администратора удалена у выбранных пользователей.");
        window.location.reload();
    });

    //// Добавление роли администратора
    //document.getElementById("addAdminSelected").addEventListener("click", async function () {
    //    const selectedRows = getSelectedRows();
    //    if (selectedRows.length !== 1) {
    //        alert("Пожалуйста, выберите одного пользователя для добавления в администраторы.");
    //        return;
    //    }
    //    const userId = selectedRows[0].querySelector(".select-row").dataset.userId;
    //    const response = await fetch(`/api/admin/addadmin?userId=${encodeURIComponent(userId)}`, {
    //        method: 'POST',
    //        headers: {
    //            'Content-Type': 'application/json',
    //            'RequestVerificationToken': csrfToken
    //        }
    //    });
    //    if (response.redirected) {
    //        window.location.href = response.url;
    //        return;
    //    }
    //    alert("Пользователь добавлен в администраторы.");
    //    window.location.reload();
    //});

    //// Удаление роли администратора
    //document.getElementById("removeAdminSelected").addEventListener("click", async function () {
    //    const selectedRows = getSelectedRows();
    //    if (selectedRows.length !== 1) {
    //        alert("Пожалуйста, выберите одного пользователя для удаления из администраторов.");
    //        return;
    //    }
    //    const userId = selectedRows[0].querySelector(".select-row").dataset.userId;
    //    const response = await fetch(`/api/admin/removeadmin?userId=${encodeURIComponent(userId)}`, {
    //        method: 'POST',
    //        headers: {
    //            'Content-Type': 'application/json',
    //            'RequestVerificationToken': csrfToken
    //        }
    //    });
    //    if (response.redirected) {
    //        window.location.href = response.url;
    //        return;
    //    }
    //    alert("Роль администратора удалена у пользователя.");
    //    window.location.reload();
    //});

    // Сортировка по Email
    let emailSortDirection = 0; // 0: unsorted, 1: ascending, -1: descending
    document.getElementById("sortEmailButton").addEventListener("click", function () {
        emailSortDirection = emailSortDirection === 1 ? -1 : 1;
        sortTableByEmail(emailSortDirection);
    });

    function sortTableByEmail(direction) {
        const tableBody = document.querySelector("#userTable tbody");
        const rows = Array.from(tableBody.querySelectorAll("tr"));

        rows.sort((a, b) => {
            const emailA = a.cells[2].innerText.toLowerCase();
            const emailB = b.cells[2].innerText.toLowerCase();
            if (emailA < emailB) return direction === 1 ? -1 : 1;
            if (emailA > emailB) return direction === 1 ? 1 : -1;
            return 0;
        });
        rows.forEach(row => tableBody.appendChild(row));
        const sortIcon = document.getElementById("sortEmailIcon");
        sortIcon.textContent = direction === 1 ? "⬆" : "⬇";
    }
});