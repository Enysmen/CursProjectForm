import { Base } from "survey-core";
import { SidebarModel } from "./side-bar-model";
export declare class SidebarPageModel extends Base {
    id: string;
    sidePanel: SidebarModel;
    caption: string;
    visible: boolean;
    componentData: any;
    componentName: string;
    deactivateCallback: () => void;
    constructor(id: string, sidePanel: SidebarModel, componentName?: string, componentData?: any);
}
