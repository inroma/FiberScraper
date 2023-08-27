import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import FiberPointDTO from '@/models/FiberPointDTO';

@Component({
})
export default class LeafletPopupContent extends Vue {
    //#region Public Properties

    @Prop({ required: true, default: null, type: FiberPointDTO})
    public fiber?: FiberPointDTO;
    
    @Prop({ required: true, default: false, type: Boolean})
    public loading?: boolean;

    //#endregion
}