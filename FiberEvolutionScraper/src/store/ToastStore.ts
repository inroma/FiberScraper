import { ISnackbar, ISnackbarColor, Snackbar } from "@/models/SnackbarInterface";
import { defineStore } from 'pinia';

const defaultState = {
    snackbars: [] as Snackbar[],
    index: 0
};

export const useToastStore = defineStore('toast', {
    state: (() => {
        return {...defaultState};
    }),
    actions: {
        createToastMessage(params: ISnackbar) {
            this.snackbars.unshift(new Snackbar(
                this.index,
                // On insert des Snack avec show à null pour avoir l'animation d'apparition quand il passe à true
                null,
                params.message || '',
                params.color || ISnackbarColor.Info,
                params.timeout || 8000,
                params.icon
            ));
            this.index += 1;
        },
        removeToastMessages() {
            this.snackbars = this.snackbars.filter(s => s.show === null || s.show);
        }
    },
    getters: {
        getToasts(state): Snackbar[] {
            return state.snackbars;
        }
    }
})