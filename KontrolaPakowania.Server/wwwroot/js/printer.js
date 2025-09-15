window.sendZplToAgent = async function (printer, dataType, content) {
    try {
        const controller = new AbortController();
        const timeout = setTimeout(() => controller.abort(), 3000);

        const response = await fetch('http://localhost:54321/print/', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ PrinterName: printer, DataType: dataType, Content: content }),
            signal: controller.signal
        });

        clearTimeout(timeout);

        console.log(`[INFO] Agent response status: ${response.status}`);
        return response.ok;
    } catch (err) {
        console.error("[ERROR] Failed to send print job to agent:", err);
        return false;
    }
};

window.downloadAgent = (url, filename) => {
    const a = document.createElement("a");
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
};

window.isAgentRunning = async () => {
    try {
        const response = await fetch("http://localhost:54321/print/", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ PrinterName: "test", DataType: "ZPL", Content: "" })
        });
        return response.ok; // 200 OK means agent is alive
    } catch {
        return false; // Cannot connect
    }
};