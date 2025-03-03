


//// Опции конструктора: включаем, например, вкладку логики и отключаем авто-сохранение
//var creatorOptions = {
//    showLogicTab: true,
//    isAutoSave: false
//};

//// Создаем экземпляр SurveyJS Creator с заданными опциями
//var creator = new SurveyCreator.SurveyCreator(creatorOptions);

//// Попытка распарсить начальную JSON-схему из скрытого поля
//var jsonSchemaString = document.getElementById("JsonSchema").value;
//var jsonSchema;
//try {
//    jsonSchema = JSON.parse(jsonSchemaString);
//} catch (e) {
//    console.error("Ошибка парсинга JSON-схемы: ", e);
//    jsonSchema = { title: "Новый опрос", description: "Введите описание опроса", elements: [] };
//}
//// Загружаем JSON-схему в конструктор
//creator.JSON = jsonSchema;

//// При событии DOMContentLoaded рендерим конструктор в контейнере
//document.addEventListener("DOMContentLoaded", function () {
//    creator.render("surveyCreatorContainer");
//});

//// При нажатии на кнопку «Сохранить шаблон»
//document.getElementById("saveFormBtn").addEventListener("click", function () {
//    // Извлекаем обновленную JSON-схему
//    var updatedSchema = creator.JSON;
//    // Сериализуем схему и сохраняем её в скрытом поле
//    document.getElementById("JsonSchema").value = JSON.stringify(updatedSchema);
//    // Отправляем форму
//    document.getElementById("templateForm").submit();
//});


var creatorOptions = {
    showLogicTab: true,
    isAutoSave: false
};

var creator = new SurveyCreator.SurveyCreator(creatorOptions);

// Попытка распарсить начальную JSON-схему из скрытого поля
var jsonSchemaString = document.getElementById("JsonSchema").value;
var jsonSchema;
try {
    jsonSchema = JSON.parse(jsonSchemaString);
} catch (e) {
    console.error("Ошибка парсинга JSON-схемы: ", e);
    jsonSchema = { title: "Новый опрос", description: "Введите описание опроса", elements: [] };
}
creator.JSON = jsonSchema;

document.addEventListener("DOMContentLoaded", function () {
    creator.render("surveyCreatorContainer");
});

document.getElementById("saveFormBtn").addEventListener("click", function () {
    var updatedSchema = creator.JSON;
    document.getElementById("JsonSchema").value = JSON.stringify(updatedSchema);
    document.getElementById("templateForm").submit();
});