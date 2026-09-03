import fs from "node:fs/promises";
import path from "node:path";
import { FileBlob, SpreadsheetFile } from "@oai/artifact-tool";

const root = path.resolve(import.meta.dirname, "..", "..");
const qaDir = path.join(root, "QA");
const outDir = path.join(import.meta.dirname, "extracted");
await fs.mkdir(outDir, { recursive: true });

const files = (await fs.readdir(qaDir))
  .filter((name) => name.toLowerCase().endsWith(".xlsx"))
  .sort();

const manifest = [];

for (const fileName of files) {
  const filePath = path.join(qaDir, fileName);
  const workbook = await SpreadsheetFile.importXlsx(await FileBlob.load(filePath));
  const sheetInspection = await workbook.inspect({
    kind: "sheet",
    include: "id,name",
    maxChars: 20000,
  });

  const sheetRows = sheetInspection.ndjson
    .split(/\r?\n/)
    .filter(Boolean)
    .map((line) => JSON.parse(line))
    .filter((row) => row.name);

  const workbookResult = {
    fileName,
    sheetInspection: sheetRows,
    sheets: [],
  };

  for (const sheetRow of sheetRows) {
    const sheet = workbook.worksheets.getItem(sheetRow.name);
    const usedRange = sheet.getUsedRange();
    const values = usedRange ? usedRange.values : [];
    const formulas = usedRange ? usedRange.formulas : [];
    const address = usedRange ? usedRange.address : null;

    const safeName = sheetRow.name.replace(/[<>:"/\\|?*]/g, "_");
    const render = await workbook.render({
      sheetName: sheetRow.name,
      autoCrop: "all",
      scale: 1,
      format: "png",
    });
    const renderPath = path.join(
      outDir,
      `${path.parse(fileName).name}__${safeName}.png`,
    );
    await fs.writeFile(renderPath, new Uint8Array(await render.arrayBuffer()));

    workbookResult.sheets.push({
      name: sheetRow.name,
      id: sheetRow.id,
      address,
      values,
      formulas,
      renderPath,
    });
  }

  const outputPath = path.join(outDir, `${path.parse(fileName).name}.json`);
  await fs.writeFile(outputPath, JSON.stringify(workbookResult, null, 2), "utf8");
  manifest.push({
    fileName,
    outputPath,
    sheets: workbookResult.sheets.map((sheet) => ({
      name: sheet.name,
      address: sheet.address,
      rows: sheet.values.length,
      columns: Math.max(0, ...sheet.values.map((row) => row.length)),
      renderPath: sheet.renderPath,
    })),
  });
}

await fs.writeFile(
  path.join(outDir, "manifest.json"),
  JSON.stringify(manifest, null, 2),
  "utf8",
);

console.log(JSON.stringify(manifest, null, 2));
