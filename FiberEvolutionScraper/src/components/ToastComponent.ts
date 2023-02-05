import { Component } from 'vue-property-decorator';
import Vue from 'vue';
import { ToastStoreMethods } from '@/store/ToastStore';
import { Getter } from 'vuex-class';
import { ISnackbarColor, Snackbar } from '@/models/SnackbarInterface';

@Component({})
export default class ToastComponent extends Vue {
    @Getter(ToastStoreMethods.GET_TOAST)
    public snackbars!: Snackbar[];

    public hideToast(snack: Snackbar) {
        snack.show = false;
    }

    public getIcon(snack: Snackbar) {
        let icon = "";
        if (snack.icon !== undefined) {
            icon = snack.icon;
        } else {
            switch (snack.color) {
                case ISnackbarColor.Success: 
                    icon = "mdi-check";
                    break;
                case ISnackbarColor.Info: 
                    icon = "mdi-information-outline";
                    break;
                case ISnackbarColor.Warning: 
                    icon = "mdi-alert-circle-outline";
                    break;
                case ISnackbarColor.Error: 
                    icon = "mdi-alert-outline";
                    break;
                default:
                    icon = "mdi-information-outline";
                    break;
            }
        }
        return icon;
    }
}