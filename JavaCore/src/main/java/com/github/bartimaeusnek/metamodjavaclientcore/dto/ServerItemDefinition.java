package com.github.bartimaeusnek.metamodjavaclientcore.dto;

import com.google.gson.JsonDeserializationContext;
import com.google.gson.JsonDeserializer;
import com.google.gson.JsonElement;
import com.google.gson.JsonParseException;
import lombok.Data;
import lombok.var;

import java.lang.reflect.Type;

@Data
public class ServerItemDefinition {
    public String UniqueIdentifier;

    public static class Deserializer implements JsonDeserializer<ServerItemDefinition> {
        @Override
        public ServerItemDefinition deserialize(JsonElement json, Type typeOfT, JsonDeserializationContext context) throws JsonParseException {
            var obj = json.getAsJsonObject();
            var uniqueIdentifier= obj.get("uniqueIdentifier").getAsString();
            var ret = new ServerItemDefinition();
            ret.setUniqueIdentifier(uniqueIdentifier);
            return ret;
        }
    }
}
