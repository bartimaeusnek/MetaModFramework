package com.github.bartimaeusnek.metamodjavaclientcore.dto;

import com.google.gson.*;
import lombok.Data;
import lombok.var;

import java.lang.reflect.Type;
import java.util.Locale;

@Data
public class ServerItem {
    public ServerItemDefinition ItemDefinition;
    public long Amount;

    public static class Deserializer implements JsonDeserializer<ServerItem> {
        @Override
        public ServerItem deserialize(JsonElement json, Type typeOfT, JsonDeserializationContext context) throws JsonParseException {
            var gson = new GsonBuilder()
                    .setFieldNamingStrategy(f -> f.getName().toLowerCase(Locale.ROOT))
                    .registerTypeAdapter(ServerItemDefinition.class, new ServerItemDefinition.Deserializer())
                    .create();
            var obj = json.getAsJsonObject(); //our original full json string
            var dataElement = obj.get("ItemDefinition");
            var cid = gson.fromJson(dataElement, ServerItemDefinition.class);
            var amount = obj.get("Amount").getAsLong();
            var ret = new ServerItem();
            ret.setAmount(amount);
            ret.setItemDefinition(cid);
            return ret;
        }
    }
}
