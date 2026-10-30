// Get references to the HTML elements that the script will update.
const documentList = document.getElementById("document-list");
const documentCount = document.getElementById("document-count");
const search = document.getElementById("search");

// Read the document data embedded into index.html by the C# generator.
// If packageData or documents does not exist, fall back to an empty array.
const documents = window.packageData?.documents ?? [];

// Displays the documents on the page.
// An optional filter is used when the user types into the search box.
function renderDocuments(filter = "") {

    // Normalise the search text so matching is case-insensitive
    // and ignores leading/trailing spaces.
    const searchText = filter.toLowerCase().trim();

    // Create a new array containing only documents whose filename
    // or document type contains the search text.
    const filteredDocuments = documents.filter(document =>
        document.fileName.toLowerCase().includes(searchText) ||
        document.type.toLowerCase().includes(searchText)
    );

    // Clear the current list before rebuilding it.
    documentList.innerHTML = "";

    // Create an HTML element for each matching document
    // and add it to the document list.
    for (const item of filteredDocuments) {
        documentList.appendChild(createDocumentItem(item));
    }

    // Update the displayed document count.
    documentCount.textContent =
        `${filteredDocuments.length} document(s)`;
}

// Creates the HTML structure for one document row/card.
function createDocumentItem(item) {

    // Outer container for the document.
    //
    // Produces:
    // <div class="document">...</div>
    const container = document.createElement("div");
    container.className = "document";

    // Container for the filename and document type.
    const details = document.createElement("div");

    // Create the filename element.
    const title = document.createElement("strong");
    title.textContent = item.fileName;

    // Create the document type element.
    const type = document.createElement("span");
    type.textContent = item.type;

    // Add the filename and type to the details container.
    details.appendChild(title);
    details.appendChild(type);

    // Create the link that opens the document on the USB.
    const link = document.createElement("a");
    link.href = item.path;
    link.textContent = "Open";

    // Assemble the complete document item.
    container.appendChild(details);
    container.appendChild(link);

    return container;
}

// Listen for changes to the search box.
// "input" fires whenever the user types, deletes or pastes text.
search.addEventListener("input", event => {
    renderDocuments(event.target.value);
});

// Initial render when the page first opens.
// No filter means all documents are displayed.
renderDocuments();