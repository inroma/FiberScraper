<script setup lang="ts">
import router from '@/router';
import { HttpClient } from '@/shared/HttpClient';
import { auth } from '@/shared/services/auth/OAuthService';

onMounted(async () => {
  const user = await auth().getUser();
  if (!user) {
    await auth().signInRedirect();
  } else {
    HttpClient.setTokenHeader(user.access_token);
    router.push("/");
  }
});

</script>
<template>

</template>