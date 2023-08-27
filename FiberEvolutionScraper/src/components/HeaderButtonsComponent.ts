import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';

@Component({
})
export default class HeaderButtonsComponent extends Vue {
    //#region Public Properties

    @Prop({})
    public loading?: boolean;

    //#endregion

    public updateFibers() {
        this.$emit('updateFibers');
    }
    
    public getDbFibers() {
        this.$emit('getDbFibers');
    }
    
    public getFibers() {
        this.$emit('getFibers');
    }
    
    public getCloseAreaFibers() {
        this.$emit('getCloseAreaFibers');
    }
    
    public getNewestFibers() {
        this.$emit('getNewestFibers');
    }
}