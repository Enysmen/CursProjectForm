import { SurveyCreatorModel } from "../../creator-base";
import { PreviewViewModel } from "./preview";
export declare class TestSurveyTabViewModel extends PreviewViewModel {
    constructor(surveyProvider: SurveyCreatorModel, startThemeClasses: any);
    protected getTabName(): string;
    protected getShowResults(): boolean;
}
