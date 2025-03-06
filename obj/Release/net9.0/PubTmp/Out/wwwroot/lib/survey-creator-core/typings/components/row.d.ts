import { Base, SurveyTemplateRendererTemplateData, QuestionRowModel, DragTypeOverMeEnum } from "survey-core";
import { SurveyCreatorModel } from "../creator-base";
export declare class RowViewModel extends Base {
    creator: SurveyCreatorModel;
    row: QuestionRowModel;
    templateData: SurveyTemplateRendererTemplateData;
    constructor(creator: SurveyCreatorModel, row: QuestionRowModel, templateData: SurveyTemplateRendererTemplateData);
    subscribeElementChanges(): void;
    unsubscribeElementChanges(): void;
    dragTypeOverMe: DragTypeOverMeEnum;
    private rowDragTypeOverMeChanged;
    get cssClasses(): string;
    dispose(): void;
}
