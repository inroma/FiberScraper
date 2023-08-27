import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import ToastComponent from './components/ToastComponent.vue';

@Component({
    components: {
      'toast': ToastComponent
    }
})
export default class FiberMapVue extends Vue {
  public get headers() {
    return [
        {
          title: 'Home',
          icon: 'mdi-home-outline',
          url: '/home',
        },
        {
          title: 'Deployment',
          icon: 'mdi-connection',
          url: '/',
        },
        {
          title: 'About',
          icon: 'mdi-help-box',
          url: '/about',
        },
    ]
  }

  public drawer: boolean = false;

  public get smallScreen(): boolean {
    return this.$vuetify.breakpoint.mdAndDown;
  }
}