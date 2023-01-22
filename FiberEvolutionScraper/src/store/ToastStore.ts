import { ActionTree, GetterTree, Module, MutationTree } from "vuex";
import { RootState } from "./RootState";
import { Snackbar } from "@/models/SnackbarInterface";

export enum ToastStoreMethods {
    CREATE_TOAST_MESSAGE = 'CREATE_TOAST_MESSAGE',
    GET_TOAST = 'GET_TOAST',
    GET_TOAST_MESSAGE = 'GET_TOAST_MESSAGE'
}

export class ToastState {
    public snackbar: Snackbar = {
        show: false,
        message: '',
        color: "",
        timeout: -1
    }
}

const getters: GetterTree<ToastState, RootState> = {
    [ToastStoreMethods.GET_TOAST](state) { return state.snackbar; }
}

const actions: ActionTree<ToastState, RootState> = {
    [ToastStoreMethods.CREATE_TOAST_MESSAGE]: (state, params) => {
        state.commit(ToastStoreMethods.CREATE_TOAST_MESSAGE, params);
    }
}

const mutations: MutationTree<ToastState> = {
    [ToastStoreMethods.CREATE_TOAST_MESSAGE](state, snackbar: Snackbar) {
        state.snackbar.show = true;
        state.snackbar.message = snackbar.message || '';
        state.snackbar.timeout = snackbar.timeout || 3500;
        state.snackbar.color = snackbar.color || "info";
    }
}

export const toastStore: Module<ToastState, RootState> = {
    state: new ToastState(),
    getters,
    mutations,
    actions,
    namespaced: false
}