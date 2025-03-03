import { CreatorBase } from "./creator-base";
import { SurveyElementAdornerBase } from "./components/action-container-view-model";
import { SurveyElement } from "survey-core";
import { ElementGetExpandCollapseStateEventReason } from "./creator-events-api";
export declare class ExpandCollapseManager {
    private creator;
    private _lockQuestions;
    constructor(creator: CreatorBase);
    expandCollapseElements(reason: ElementGetExpandCollapseStateEventReason, isCollapsed: boolean, elements?: SurveyElement[]): void;
    get questionsLocked(): boolean;
    lockQuestions(locked: boolean): void;
    private getCollapsableElements;
    private updateCollapsed;
    private adorners;
    add(adorner: SurveyElementAdornerBase): void;
    remove(adorner: SurveyElementAdornerBase): void;
    clear(): void;
}
