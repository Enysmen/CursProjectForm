// Функция для загрузки JSON из скрытого поля
var jsonSchemaString = document.getElementById("JsonSchema").value;
var jsonSchema;
try {
    jsonSchema = JSON.parse(jsonSchemaString);
} catch (e) {
    console.error("Ошибка парсинга JSON-схемы: ", e);
    jsonSchema = { title: "Новый опрос", description: "Введите описание опроса", elements: [] };
}

var creatorOptions = {
    showLogicTab: true,
    isAutoSave: false
};

var creator = new SurveyCreator.SurveyCreator(creatorOptions);
creator.JSON = jsonSchema;

document.addEventListener("DOMContentLoaded", function () {
    creator.render("surveyCreatorContainer");
});

// Сохранение изменений
document.getElementById("saveFormBtn").addEventListener("click", function () {
    var updatedSchema = creator.JSON;
    // Обновляем скрытое поле
    document.getElementById("JsonSchema").value = JSON.stringify(updatedSchema);
    // Обновляем Title и Description из JSON, если заданы
    if (updatedSchema.title) {
        // Здесь мы обновляем скрытые поля модели, но так как у нас нет отдельных input для Title,
        // можно добавить их как скрытые поля или обновить модель через JS, если это необходимо.
        // Для примера, можно использовать следующий код, если вы добавите скрытые поля:
        // document.getElementById("Title").value = updatedSchema.title;
        // document.getElementById("Description").value = updatedSchema.description;
    }
    document.getElementById("templateForm").submit();
});