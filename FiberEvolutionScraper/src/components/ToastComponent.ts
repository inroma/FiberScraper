import { Component } from 'vue-property-decorator';
import Vue from 'vue';
import { ToastStoreMethods } from '@/store/ToastStore';
import { Getter } from 'vuex-class';
import { Snackbar } from '@/models/SnackbarInterface';

@Component({})
export default class ToastComponent extends Vue {
    @Getter(ToastStoreMethods.GET_TOAST)
    public snackbars!: Snackbar[];

    public icon(snackbar: Snackbar) {
        let icon = "";
        switch (snackbar?.color){
            default:
                break;
            case "success": 
                icon = "mdi-check";
                break;
            case "info": 
                icon = "mdi-information-outline";
                break;
            case "warning": 
                icon = "mdi-alert-circle-outline";
                break;
            case "error": 
                icon = "mdi-alert-outline";
                break;
        }
        return icon;
    }

    public hideToast(snack: Snackbar) {
        snack.show = false;
    }
}