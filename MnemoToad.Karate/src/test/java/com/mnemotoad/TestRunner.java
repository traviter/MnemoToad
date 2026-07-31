package com.mnemotoad;

import com.intuit.karate.Runner;
import com.intuit.karate.Results;
import org.junit.jupiter.api.Test;

import java.util.Arrays;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;

class TestRunner {

    @Test
    void testParallel() {
        String tagsProperty = System.getProperty("karate.tags");

        Runner.Builder<?> builder = Runner.path("classpath:com/mnemotoad")
                .outputCucumberJson(true);

        if (tagsProperty != null && !tagsProperty.isBlank()) {
            List<String> tags = Arrays.asList(tagsProperty.split(","));
            builder = builder.tags(tags);
        }

        Results results = builder.parallel(5);
        assertEquals(0, results.getFailCount(), results.getErrorMessages());
    }
}
