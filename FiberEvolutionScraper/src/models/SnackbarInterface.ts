export interface ISnackbar {
    message: string,
    color?: ISnackbarColor | string,
    timeout?: number,
    icon?: string | undefined
}

export const enum ISnackbarColor {
    Success = 'success',
    Info = 'info',
    Warning = 'warning',
    Error = 'error'
}

export class Snackbar implements ISnackbar {
    show?: boolean;
    message = "";
    color: ISnackbarColor | string;
    timeout: number = 4000;
    showtime: number;
    icon: string | undefined;

    /**
     * Constructeur d'un Snackbar
     */
    constructor(show: boolean, message: string, color: ISnackbarColor | string, timeout: number = 4000, icon: string | undefined = undefined) {
        this.show = show,
        this.message = message,
        this.timeout = timeout,
        this.color = color,
        this.showtime = timeout,
        this.icon = icon
    }
}