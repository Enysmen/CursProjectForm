import { QuestionImageModel, SurveyElement, SurveyTemplateRendererTemplateData, QuestionFileModel } from "survey-core";
import { SurveyCreatorModel } from "../creator-base";
import { QuestionAdornerViewModel } from "./question";
export declare class QuestionImageAdornerViewModel extends QuestionAdornerViewModel {
    private editorSurveyModel;
    filePresentationModel: QuestionFileModel;
    private initFilePresentationModel;
    constructor(creator: SurveyCreatorModel, surveyElement: SurveyElement, templateData: SurveyTemplateRendererTemplateData);
    isUploading: any;
    isEmptyImageLink: any;
    chooseFile(model: QuestionImageAdornerViewModel): void;
    get acceptedTypes(): string;
    imageLinkValueChangedHandler: () => void;
    get isEmptyElement(): boolean;
    get question(): QuestionImageModel;
    get placeholderText(): string;
    get chooseImageText(): string;
    protected getAnimatedElement(): Element;
    css(): string;
    dispose(): void;
}
