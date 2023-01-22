import { ActionTree, GetterTree, Module, MutationTree } from "vuex";
import { RootState } from "./RootState";
import { Snackbar } from "@/models/SnackbarInterface";

export enum ToastStoreMethods {
    CREATE_TOAST_MESSAGE = 'CREATE_TOAST_MESSAGE',
    GET_TOAST = 'GET_TOAST',
    REMOVE_TOAST_MESSAGE = 'REMOVE_TOAST_MESSAGE'
}

export class ToastState {
    public snackbars: Snackbar[] = [];
    public index = 0;
}

const getters: GetterTree<ToastState, RootState> = {
    [ToastStoreMethods.GET_TOAST](state) { return state.snackbars; }
}

const actions: ActionTree<ToastState, RootState> = {
    [ToastStoreMethods.CREATE_TOAST_MESSAGE]: (state, params) => {
        state.commit(ToastStoreMethods.CREATE_TOAST_MESSAGE, params);
        state.state.index += 1;
    },
    [ToastStoreMethods.REMOVE_TOAST_MESSAGE]: (state) => {
        state.commit(ToastStoreMethods.REMOVE_TOAST_MESSAGE);
    },
}

const mutations: MutationTree<ToastState> = {
    [ToastStoreMethods.CREATE_TOAST_MESSAGE](state, snackbar: Snackbar) {
        state.snackbars.push({
            id: state.index,
            show: null,
            message: snackbar.message || '',
            timeout: snackbar.timeout || 5000,
            color: snackbar.color || "info"
        });
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