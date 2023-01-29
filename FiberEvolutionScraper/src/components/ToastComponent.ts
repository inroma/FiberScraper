import { Component, Watch } from 'vue-property-decorator';
import Vue from 'vue';
import { ToastStoreMethods } from '@/store/ToastStore';
import { Action, Getter } from 'vuex-class';
import { Snackbar } from '@/models/SnackbarInterface';

@Component({})
export default class ToastComponent extends Vue {
    @Getter(ToastStoreMethods.GET_TOAST)
    public snackbars!: Snackbar[];

    @Action(ToastStoreMethods.REMOVE_TOAST_MESSAGE)
    private clearToast!: () => void;

    @Watch('snackbars', { deep: true })
    public showToast() {
        this.$nextTick(() => {
            if (this.snackbars.length > 0) {
                this.snackbars.forEach(snack => {
                    if (snack.show === null){
                        snack.show = true;
                        this.refreshProgressBar(snack);
                    }
                });
                if (this.snackbars.filter(s => !s.show).length > 0) {
                    setTimeout(() => this.clearToast(), 300);
                }
            }
        });
    }

    private refreshProgressBar(snack: Snackbar) {
        snack.funcTimeout = setTimeout(() => {
            //Increment the time counter by 100
            // eslint-disable-next-line
            snack.showtime! -= 100;

            if (snack.show) {
                //Recursively update the progress bar
                this.refreshProgressBar(snack);
            }
        }, 100);
    }

    private clearTimeout(snack : Snackbar) {
        snack.showtime = snack.timeout;
        clearTimeout(snack.funcTimeout); // TODO
    }

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

    public removeToast(snack: Snackbar) {
        snack.show = false;
    }
}