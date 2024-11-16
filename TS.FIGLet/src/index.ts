import { FIGFont } from "./FIGLet/FIGFont.js";
import { FIGLetRenderer } from "./FIGLet/FIGLetRenderer.js";

async function main() {
    const fnt = await FIGFont.getDefault();
    const rndr = new FIGLetRenderer(fnt);
    const result = rndr.render("TypeScript");
    console.log(result);
}

// Run the main function
main().catch(error => {
    console.error(error);
    process.exit(1);
});