<script setup lang="ts">
import router from '@/router';
import { userService } from '@/services/UserService';
import { useAuth } from '@/shared/composables/OAuthComposable';
import { useUserStore } from '@/store/userStore';

const auth = useAuth();
const userStore = useUserStore();

onMounted(async () => {
	if (!await auth.getUser()) {
		await auth.signInCallback()
		.then(async () => {
			const user = await userService.syncUser();
			userStore.user = user.data;
		})
		.catch(() => auth.isConnected.value = false);
	}
	auth.isConnected.value = true;
	router.push("/");
});

</script>
<template>
  <VDialog persistent no-click-animation :scrim="false" :model-value="true" location="center" width="auto">
    <VCard class="px-10 py-3">
      Connexion en cours...
    </VCard>
  </VDialog>
</template>