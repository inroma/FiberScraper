<script setup lang="ts">
import router from '@/router';
import { useAuth } from '@/shared/composables/OAuthComposable';

const auth = useAuth();

onMounted(async () => {
  const user = await auth.getUser();
  if (!user) {
    await auth.signInRedirect();
  } else {
	  auth.isConnected.value = true;
    router.push("/");
  }
});

</script>
<template>
  <VDialog persistent no-click-animation :scrim="false" :model-value="true" location="center" width="auto">
    <VCard class="px-10 py-3">
      Connexion en cours...
    </VCard>
  </VDialog>
</template>