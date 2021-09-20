package com.github.bartimaeusnek.metamodjavaclientcore.dto;

import com.google.gson.JsonDeserializationContext;
import com.google.gson.JsonDeserializer;
import com.google.gson.JsonElement;
import com.google.gson.JsonParseException;
import lombok.Data;
import lombok.var;

import java.lang.reflect.Type;

@Data
public class ClientItemDefinition {
    public String UniqueIdentifier;
    public String Game;

    public static class Deserializer implements JsonDeserializer<ClientItemDefinition> {

        @Override
        public ClientItemDefinition deserialize(JsonElement json, Type typeOfT, JsonDeserializationContext context) throws JsonParseException {
            var obj = json.getAsJsonObject();
            var uniqueIdentifier= obj.get("uniqueIdentifier").getAsString();
            var game = obj.get("game").getAsString();
            var ret = new ClientItemDefinition();
            ret.setGame(game);
            ret.setUniqueIdentifier(uniqueIdentifier);
            return ret;
        }
    }
}
