<script setup lang="ts">
import router from '@/router';
import { HttpClient } from '@/shared/HttpClient';
import { auth } from '@/shared/services/auth/OAuthService';

onMounted(async () => {
	if (!await auth().getUser()) {
		await auth().signInCallback()
		.then((val) => {
			HttpClient.setTokenHeader(val.access_token);
			router.push("/");
		});
	}
});

</script>
<template>

</template>