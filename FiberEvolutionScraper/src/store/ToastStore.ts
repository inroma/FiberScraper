import { ActionTree, GetterTree, Module, MutationTree } from "vuex";
import VSnackbar from "vuetify";
import { RootState } from "./RootState";

export enum ToastStoreMethods {
    CREATE_TOAST_MESSAGE = 'CREATE_TOAST_MESSAGE',
    REMOVE_TOAST_MESSAGE = 'REMOVE_TOAST_MESSAGE'
}

export class ToastState {
    public toast: VSnackbar = new VSnackbar();
}

const getters: GetterTree<ToastState, RootState> = {
}

const actions: ActionTree<ToastState, RootState> = {
}

const mutations: MutationTree<ToastState> = {
    [ToastStoreMethods.CREATE_TOAST_MESSAGE]: (state, mutation) => {
        state.toast = mutation.toast;
        //
    },
    [ToastStoreMethods.REMOVE_TOAST_MESSAGE]: () => {
        //
    }
}

export const toastStore: Module<ToastState, RootState> = {
    state: new ToastState(),
    getters,
    mutations,
    actions,
    namespaced: false
}