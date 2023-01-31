import { ActionTree, GetterTree, Module, MutationTree } from "vuex";
import { RootState } from "./RootState";
import { ISnackbar, Snackbar } from "@/models/SnackbarInterface";

export enum ToastStoreMethods {
    CREATE_TOAST_MESSAGE = 'CREATE_TOAST_MESSAGE',
    GET_TOAST = 'GET_TOAST',
    REMOVE_TOAST_MESSAGE = 'REMOVE_TOAST_MESSAGE'
}

export class ToastState {
    public snackbars: Snackbar[] = [];
    public index = 0;

    _ = setInterval(() => {
          if (this.snackbars.length > 0) {
            // On insert des Snack avec show à null pour avoir l'animation d'apparition quand il passe à true
            this.snackbars.filter(s => s.show === null).forEach(snack => {
                snack.show = true;
            });
          }
          this.snackbars.forEach(snack => {
              this.refreshProgressBar(snack);
          });
        }, 100);

    private refreshProgressBar(snack: Snackbar) {
        if (!snack.mouseOver && snack.showtime! > 0) {
          snack.showtime -= 100;
        }
    }

    /**
     * Vide le tableau des notifs expirées
     */
    private __ = setInterval(() => {
        if (this.snackbars.filter(s => !s.show).length > 0) {
            this.snackbars = this.snackbars.filter(s => s.show === null || s.show);
        }
    }, 240);
}

const getters: GetterTree<ToastState, RootState> = {
    [ToastStoreMethods.GET_TOAST](state) { return state.snackbars; }
}

const actions: ActionTree<ToastState, RootState> = {
    [ToastStoreMethods.CREATE_TOAST_MESSAGE]: (state, params: ISnackbar) => {
        state.commit(ToastStoreMethods.CREATE_TOAST_MESSAGE, params);
        state.state.index += 1;
    },
    [ToastStoreMethods.REMOVE_TOAST_MESSAGE]: (state) => {
        state.commit(ToastStoreMethods.REMOVE_TOAST_MESSAGE);
    },
}

const mutations: MutationTree<ToastState> = {
    [ToastStoreMethods.CREATE_TOAST_MESSAGE](state, snackbar: ISnackbar) {
        state.snackbars.unshift(new Snackbar(
            state.index,
            // On insert des Snack avec show à null pour avoir l'animation d'apparition quand il passe à true
            null,
            snackbar.message || '',
            snackbar.color || "info",
            snackbar.timeout || 8000
        ));
    },
    [ToastStoreMethods.REMOVE_TOAST_MESSAGE](state) {
        state.snackbars = state.snackbars.filter(s => s.show === null || s.show);
    }
}

export const toastStore: Module<ToastState, RootState> = {
    state: new ToastState(),
    getters,
    mutations,
    actions,
    namespaced: false
}