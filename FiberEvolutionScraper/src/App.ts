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
          title: 'Accueil',
          icon: 'mdi-home-outline',
          url: '/home',
          disabled: false
        },
        {
          title: 'Déploiements',
          icon: 'mdi-connection',
          url: '/',
          disabled: false
        },
        {
          title: 'Auto-Refresh',
          icon: 'mdi-timer-refresh-outline',
          url: '/auto-refresh',
          disabled: false
        },
        {
          title: 'About',
          icon: 'mdi-help-box',
          url: '/about',
          disabled: false
        },
    ]
  }

  public drawer: boolean = !this.smallScreen;

  public get smallScreen(): boolean {
    return this.$vuetify.breakpoint.mdAndDown;
  }
}