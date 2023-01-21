import { Component } from 'vue-property-decorator';
import Vue from 'vue';

@Component({})
export default class ToastComponent extends Vue {
    public show = false;
    public color = "primary";
    public text = "";
    public timeout = 4000;
}