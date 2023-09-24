export default class AutoRefreshInput
{
    id: number = 0;

    enabled: boolean = true;

    label: string = "";

    coordX: number = 0.0;

    coordY: number = 0.0;

    areaSize: number = 3;

    lastRun?: Date;

    isEditing: boolean = false;
}