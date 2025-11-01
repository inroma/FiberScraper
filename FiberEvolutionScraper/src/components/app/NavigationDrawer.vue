<template>
  <VNavigationDrawer v-model="drawer" expand-on-hover class="px-0 py-0 nav-bar"
    @update:rail="(v) => isExpanded = !v" :rail="mdAndUp" :temporary="smAndDown" mobile-breakpoint="sm">
    <VList>
      <VListItem v-for="header in headers" link :to="header.url" :prepend-icon="header.icon"
        :title="header.title" :key="'header'+header.title" :disabled="header.disabled" density="default">
      </VListItem>
    </VList>
    <template #append>
      <VFadeTransition class="d-flex w-100">
        <VBtn v-if="isExpanded || smAndDown" @click="changeTheme()" height="50">
          <VIcon icon="mdi-theme-light-dark"/>
        </VBtn>
      </VFadeTransition>
    </template>
  </VNavigationDrawer>
</template>
<script setup lang="ts">
import { useDisplay, useTheme } from 'vuetify';
import { useAuth } from '@/shared/composables/OAuthComposable';

const headers = [
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
	}
];

const { smAndDown, mdAndUp } = useDisplay();
const theme = useTheme();
const isExpanded = ref(false);
const { isConnected } = useAuth();

const drawer = defineModel<boolean>("drawer");

// watchEffect(() => drawer.value = !smAndDown.value);

function changeTheme() {
  theme.change(theme.current.value.dark ? 'light' : 'dark');
}

</script>