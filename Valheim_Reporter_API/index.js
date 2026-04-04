/** **************************************************************************
 * VALHEIM REPORTER - BACKEND API (CLOUDFLARE WORKER) b1 n1h1lius
 * * [!] INSTRUCTIONS FOR SERVER ADMINISTRATORS:
 * 1. SET YOUR PASSWORD: Write your secret key in INTERNAL_SECRET_KEY below.
 * This MUST MATCH the one used in the Category Generator.
 * 2. MULTIPART SUPPORT: This Worker is designed to handle Unity's 
 * MultipartFormDataSection, which is what the Mod uses to send data.
 * 3. DEPLOY: Copy this entire script into your Cloudflare Worker editor.
 * ****************************************************************************
 * * ------------------- https://github.com/n1h1lius -------------------------*
 * **************************************************************************** */

const INTERNAL_SECRET_KEY = "12345"; // <--- WRITE YOUR PASSWORD HERE

export default {
  async fetch(request, env, ctx) {
    // Only allow POST requests (Reports sent from the Mod)
    if (request.method !== "POST") {
      console.log("API Error: Method Not Allowed. This API only accepts POST requests from the Valheim Reporter Mod.");
      return new Response("Method Not Allowed. This API only accepts POST requests from the Valheim Reporter Mod.", { status: 405 });
    }

    try {
      /**
       * 1. PARSE MULTIPART FORM DATA
       * Unity/C# sends the report using 'MultipartFormDataSection'. 
       * We extract the 'payload_json' field which contains the report details.
       */
      const formData = await request.formData();
      const payloadField = formData.get("payload_json");

      if (!payloadField) {
        console.log("API Error: Missing 'payload_json' in the request form.");
        return new Response("Backend Error: Missing 'payload_json' in the request form.", { status: 400 });
      }

      // Convert the field content to a clean string. 
      // Using .text() ensures we get the raw JSON without multipart boundaries.
      const payloadText = typeof payloadField === 'string' ? payloadField : await payloadField.text();
      
      // Parse the JSON data sent by the Mod
      const data = JSON.parse(payloadText);
      
      // The Mod sends the destination (plain or encrypted) in the "webhook" field.
      let targetWebhook = data.webhook;

      if (!targetWebhook) {
        console.log("API Error: No webhook destination found in the report payload.");
        throw new Error("No webhook destination found in the report payload.");
      }

      /**
       * 2. DECRYPTION LOGIC
       * If the webhook starts with the mod's prefix [NH]-3247, we decrypt it.
       */
      if (targetWebhook.startsWith("[NH]-3247")) {
        
        // Priority: Cloudflare Env Variable > Internal Constant
        const secretKey = env.ENCRYPTION_KEY || INTERNAL_SECRET_KEY;

        if (secretKey === "12345" || !secretKey) {
          console.log("API Error: Encryption Key not configured in the Worker settings.");
          return new Response("API Error: Encryption Key not configured in the Worker settings.", { status: 500 });
        }

        // Strip the prefix and decrypt the Base64 + XOR data
        const encryptedData = targetWebhook.replace("[NH]-3247", "");
        targetWebhook = decrypt(encryptedData, secretKey);
      }

      /**
       * 3. CLEANUP & MULTIPART RE-PACKAGING
       * We remove the 'webhook' field from the JSON data to prevent leaks.
       * Instead of sending a new JSON, we UPDATE the original formData 
       * to preserve the images (file, file2) attached by the Mod.
       */
      delete data.webhook;
      formData.set("payload_json", JSON.stringify(data));

      /**
       * 4. FORWARD TO DISCORD
       * We forward the MODIFIED formData (which includes images + cleaned JSON)
       * to the decrypted Discord Webhook URL.
       */
      const discordResponse = await fetch(targetWebhook, {
        method: "POST",
        body: formData // Forwarding the full multipart data (JSON + Images)
      });

      /**
       * 5. FINAL RESPONSE HANDLING
       * Discord usually returns 204 (No Content) on success.
       * Attempting to read a body from a 204 response causes an error in Workers.
       */
      if (discordResponse.status === 204 || discordResponse.status === 201) {
        // We return a clean 200 to the Mod to confirm everything went perfectly.
        console.log("Report successfully delivered to Discord with attachments.");
        return new Response("Report successfully delivered to Discord.", { status: 200 });
      }

      // If Discord returned an error (4xx or 5xx), we relay its message for debugging.
      const responseText = await discordResponse.text();
      return new Response(responseText || "Forwarded with non-204 status", { 
        status: discordResponse.status 
      });

    } catch (error) {
      // Catch-all for JSON parsing errors or decryption failures.
      console.log("Backend Error: " + error.message);
      return new Response("Backend Error: " + error.message, { status: 500 });
    }
  }
};

/**
 * Decryption helper: Reverses the XOR + Base64 encryption.
 * XOR logic: (char_code) ^ (key_code)
 */
function decrypt(encoded, key) {
  try {
    // Decode Base64 string to binary string
    const decoded = atob(encoded);
    let result = "";
    
    for (let i = 0; i < decoded.length; i++) {
      // XOR is its own inverse, so we apply it again with the same key.
      result += String.fromCharCode(decoded.charCodeAt(i) ^ key.charCodeAt(i % key.length));
    }
    
    return result;
  } catch (e) {
    console.log("Decryption failed. Please check if your Encryption Password is correct.");
    throw new Error("Decryption failed. Please check if your Encryption Password is correct.");
  }
}
