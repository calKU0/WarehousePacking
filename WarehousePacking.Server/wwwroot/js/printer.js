window.sendZplToAgent = async function (printer, dataType, content, parameters) {
    try {
        const body = { PrinterName: printer, DataType: dataType, Content: content, Parameters: parameters };

        // Wyślij request, ale nie czekaj na drukowanie
        const response = await fetch("http://localhost:54321/print/", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        // zwracamy natychmiast po otrzymaniu 202
        return response.status === 202;
    } catch (err) {
        console.error("Print agent error:", err);
        return false;
    }
};

window.isAgentRunning = async () => {
    try {
        const response = await fetch("http://localhost:54321/print/", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ PrinterName: "ping", DataType: "PING", Content: "" })
        });
        return response.ok;
    } catch {
        return false;
    }
};