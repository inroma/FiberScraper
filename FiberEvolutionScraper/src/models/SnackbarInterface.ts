export interface ISnackbar {
    message: string,
    color?: string | ISnackbarColor,
    timeout?: number,
}

export const enum ISnackbarColor {
    Success = 'success',
    Info = 'info',
    Warning = 'warning',
    Error = 'error'
}

export class Snackbar implements ISnackbar {
    id: number;
    show?: boolean | null;
    message = "";
    color: string | ISnackbarColor;
    timeout: number;

    mouseOver = false;
    showtime: number;

    /**
     * Constructeur d'un Snackbar
     */
    constructor(id: number, show: boolean | null, message: string, color: string | ISnackbarColor, timeout: number) {
        this.id = id,
        this.show = show,
        this.message = message,
        this.timeout = timeout,
        this.color = color,
        this.showtime = timeout
    }
}