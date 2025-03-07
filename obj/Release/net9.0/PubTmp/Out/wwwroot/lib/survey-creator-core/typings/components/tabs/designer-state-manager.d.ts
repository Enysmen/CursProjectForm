import { SurveyElement, SurveyModel } from "survey-core";
declare class ElementState {
    collapsed: boolean;
}
export declare class DesignerStateManager {
    private elementState;
    private pageState;
    private getStateMapForElement;
    private onQuestionAddedHandler;
    private onPageAddedHandler;
    private onPanelAddedHandler;
    initForSurvey(survey: SurveyModel): void;
    initForElement(element: SurveyElement): void;
    getElementState(element: SurveyElement): ElementState;
    private createElementState;
    onInitElementStateCallback: (element: SurveyElement, state: ElementState) => void;
    private _suspendCounter;
    suspend(): void;
    release(): void;
    get isSuspended(): boolean;
}
export {};
