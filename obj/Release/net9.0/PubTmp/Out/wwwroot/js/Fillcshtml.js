//// Получаем JSON-схему из ViewBag
//var jsonSchema = 'Html.Raw(ViewBag.JsonSchema);';
//var survey = new Survey.Model(jsonSchema);

//// При завершении заполнения формы отправляем данные на сервер
//survey.onComplete.add(function (result) {
//    // Отправляем данные через форму POST
//    var form = document.createElement("form");
//    form.method = "post";
//    form.action = '@Url.Action("Submit", "Survey")';

//    var templateIdInput = document.createElement("input");
//    templateIdInput.type = "hidden";
//    templateIdInput.name = "templateId";
//    templateIdInput.value = '@ViewBag.TemplateId';
//    form.appendChild(templateIdInput);

//    var responseDataInput = document.createElement("input");
//    responseDataInput.type = "hidden";
//    responseDataInput.name = "responseData";
//    responseDataInput.value = JSON.stringify(result.data);
//    form.appendChild(responseDataInput);

//    var antiForgeryInput = document.createElement("input");
//    antiForgeryInput.type = "hidden";
//    antiForgeryInput.name = "__RequestVerificationToken";
//    // Если вы используете стандартный AntiForgeryToken, убедитесь, что его значение добавлено на страницу
//    antiForgeryInput.value = '@Antiforgery.GetAndStoreTokens(HttpContext).RequestToken';
//    form.appendChild(antiForgeryInput);

//    document.body.appendChild(form);
//    form.submit();
//});
//$(function () {
//$("#surveyContainer").Survey({ model: survey });
//});


// Корректно сериализуем JSON-схему с сервера
// Используем глобальные переменные, заданные в .cshtml
var jsonSchema = window.surveyJson;
var survey = new Survey.Model(jsonSchema);

// Приводим значение isReadOnly к нижнему регистру для корректного сравнения
var isReadOnlyFlag = window.isReadOnly.toLowerCase();

// Если режим только для просмотра, отключаем ввод
if (isReadOnlyFlag === "true") {
    survey.mode = "display";
}

// Обработчик завершения заполнения формы
survey.onComplete.add(function (result) {
    if (isReadOnlyFlag === "true") {
        alert("Вы можете только просматривать форму. Для заполнения формы войдите в систему.");
        return;
    }
    var form = document.createElement("form");
    form.method = "post";
    form.action = '/Survey/Submit'; // или используйте Url.Action, если можно
    var templateIdInput = document.createElement("input");
    templateIdInput.type = "hidden";
    templateIdInput.name = "templateId";
    templateIdInput.value = window.templateId;
    form.appendChild(templateIdInput);

    var responseDataInput = document.createElement("input");
    responseDataInput.type = "hidden";
    responseDataInput.name = "responseData";
    responseDataInput.value = JSON.stringify(result.data);
    form.appendChild(responseDataInput);

    // Если используете AntiForgeryToken, его тоже нужно передать:
    // Обычно можно сгенерировать его в Razor и сохранить в глобальной переменной
    // или добавить в виде data-атрибута в элементе.
    // Пример (если вы его добавили в Razor как window.antiforgeryToken):
    if (window.antiforgeryToken) {
        var antiforgeryInput = document.createElement("input");
        antiforgeryInput.type = "hidden";
        antiforgeryInput.name = "__RequestVerificationToken";
        antiforgeryInput.value = window.antiforgeryToken;
        form.appendChild(antiforgeryInput);
    }

    document.body.appendChild(form);
    form.submit();
});
// Инициализируем SurveyJS форму в контейнере с id "surveyContainer"
$(function () {
    $("#surveyContainer").Survey({ model: survey });
});