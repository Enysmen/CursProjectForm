import { Base, JsonObjectProperty, Question } from "survey-core";
import { IPropertyEditorSetup, PropertyGridEditor } from "./index";
import { ISurveyCreatorOptions } from "../creator-settings";
export declare class PropertyGridEditorExpression extends PropertyGridEditor {
    fit(prop: JsonObjectProperty): boolean;
    getJSON(obj: Base, prop: JsonObjectProperty, options: ISurveyCreatorOptions): any;
    clearPropertyValue(obj: Base, prop: JsonObjectProperty, question: Question, options: ISurveyCreatorOptions): void;
}
export declare class PropertyGridEditorCondition extends PropertyGridEditorExpression {
    fit(prop: JsonObjectProperty): boolean;
    getJSON(obj: Base, prop: JsonObjectProperty, options: ISurveyCreatorOptions): any;
    canClearPropertyValue(obj: Base, prop: JsonObjectProperty, question: Question, options: ISurveyCreatorOptions): boolean;
    onSetup(obj: Base, question: Question, prop: JsonObjectProperty, options: ISurveyCreatorOptions): void;
    createPropertyEditorSetup(obj: Base, prop: JsonObjectProperty, question: Question, options: ISurveyCreatorOptions): IPropertyEditorSetup;
}
