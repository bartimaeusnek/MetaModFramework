package com.github.bartimaeusnek.metamodjavaclientcore.dto;

import com.google.gson.*;
import lombok.*;

import java.lang.reflect.Type;
import java.util.Locale;

@Data
public class ClientItem {
    public ClientItemDefinition ItemDefinition;
    public long Amount;

    public static class Deserializer implements JsonDeserializer<ClientItem> {
        @Override
        public ClientItem deserialize(JsonElement json, Type typeOfT, JsonDeserializationContext context) throws JsonParseException {
            var gson = new GsonBuilder()
                    .setFieldNamingStrategy(f -> f.getName().toLowerCase(Locale.ROOT))
                    .registerTypeAdapter(ClientItemDefinition.class, new ClientItemDefinition.Deserializer())
                    .create();
            var obj = json.getAsJsonObject(); //our original full json string
            var dataElement = obj.get("ItemDefinition");
            var cid = gson.fromJson(dataElement, ClientItemDefinition.class);
            var amount = obj.get("Amount").getAsLong();
            var ret = new ClientItem();
            ret.setAmount(amount);
            ret.setItemDefinition(cid);
            return ret;
        }
    }
}