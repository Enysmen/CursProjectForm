import { ActionContainer, Base } from "survey-core";
import { SidebarModel } from "./side-bar-model";
import { MenuButton } from "../../utils/actions";
export declare class TabControlModel extends Base {
    sidePanel: SidebarModel;
    topToolbar: ActionContainer<MenuButton>;
    bottomToolbar: ActionContainer<MenuButton>;
    expandCollapseAction: MenuButton;
    private updateExpandCollapseAction;
    private createToggleAction;
    constructor(sidePanel: SidebarModel);
    get sideBarClassName(): string;
}
