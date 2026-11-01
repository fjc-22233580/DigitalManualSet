
/**
 * Customer-facing Digital Documentation splash screen.
 *
 * Displays order information and a searchable list of documents
 * included in the generated documentation package.
 *
 * Document data is embedded directly into index.html by the
 * SplashScreenGenerator, allowing the page to operate offline
 * without requiring a web server or external API.
 */

// Retrieve the embedded package data from the generated HTML.
// Fall back to empty objects/arrays if the expected data is unavailable.
const packageData = window.packageData ?? {};
const documents = packageData.documents ?? [];

// Retrieve the HTML elements used to display and filter documents.
const documentList = document.getElementById("document-list");
const documentCount = document.getElementById("document-count");
const search = document.getElementById("search");

// Retrieve the order information embedded in the package.
// Populate the corresponding HTML elements, displaying "Not supplied"
// when a particular field has no value.
const order = packageData.order ?? {};

document.getElementById("order-number").textContent =
    order.number || "Not supplied";

document.getElementById("customer-name").textContent =
    order.customer || "Not supplied";

document.getElementById("system-id").textContent =
    order.systemId || "Not supplied";

document.getElementById("generated-on").textContent =
    packageData.generatedOn || "Not supplied";

/**
 * Converts internal document type names into customer-friendly labels.
 *
 * @param {string} type - The document type supplied by the application.
 * @returns {string} The document type's display name.
 */
function getTypeName(type) {
    switch (type) {
        case "Drawings":
            return "Drawing";
        case "Cert":
            return "Certificate";
        default:
            return type || "Other";
    }
}

/**
 * Displays the document collection, optionally filtered by search text.
 *
 * Searches across document number, description, filename,
 * revision and document type.
 *
 * @param {string} filter - Optional text used to filter the documents.
 */
function renderDocuments(filter = "") {

    // Normalise the search input to make matching case-insensitive
    // and ignore whitespace at the beginning or end.
    const searchText = filter.toLowerCase().trim();

    // Find documents containing the search text in any searchable field.
    const filteredDocuments = documents.filter(item => {

        // Combine the searchable document properties into one string.
        // Remove null, undefined and other falsy values before joining,
        // then convert the result to lowercase for comparison.
        const searchableText = [
            item.number,
            item.name,
            item.fileName,
            item.revision,
            getTypeName(item.type)
        ]
            .filter(Boolean)
            .join(" ")
            .toLowerCase();

        // Include the document if its searchable text contains the query.
        return searchableText.includes(searchText);
    });

    // Remove previously displayed document elements before rendering
    // the updated collection.
    documentList.replaceChildren();

    // Create and append an HTML element for each matching document.
    for (const item of filteredDocuments) {
        documentList.appendChild(createDocumentItem(item));
    }

    // Update the document counter to show the number of matching
    // documents relative to the total number in the package.
    documentCount.textContent =
        `Showing ${filteredDocuments.length} of ${documents.length}`;

    // Display a message when no documents match the search criteria.
    if (filteredDocuments.length === 0) {
        const message = document.createElement("p");
        message.className = "empty-message";
        message.textContent = "No matching documents found.";

        documentList.appendChild(message);
    }
}

/**
 * Creates the HTML structure for a single document entry.
 *
 * Each entry contains the document name, identifying metadata,
 * physical filename and a link to open the document locally.
 *
 * @param {object} item - The document metadata from the package.
 * @returns {HTMLDivElement} The constructed document element.
 */
function createDocumentItem(item) {

    // Create the outer container for the document entry.
    const container = document.createElement("div");
    container.className = "document";

    // Create a container for the document's descriptive information.
    const details = document.createElement("div");
    details.className = "document-details";

    // Display the document description as the primary title.
    // Fall back to the physical filename if no description exists.
    const title = document.createElement("strong");
    title.className = "document-title";
    title.textContent = item.name || item.fileName;

    // Create a secondary line containing the document's identifying
    // information, revision and customer-friendly document type.
    const metadata = document.createElement("div");
    metadata.className = "document-metadata";

    // Build the metadata collection, omitting missing values.
    // Only display the revision prefix when a revision is available.
    const values = [
        item.number,
        item.revision ? `Rev ${item.revision}` : null,
        getTypeName(item.type)
    ].filter(Boolean);

    // Separate metadata values with a middle-dot character.
    metadata.textContent = values.join(" · ");

    // Display the actual filename stored within the USB package.
    const filename = document.createElement("small");
    filename.className = "document-filename";
    filename.textContent = item.fileName;

    // Assemble the descriptive section of the document entry.
    details.appendChild(title);
    details.appendChild(metadata);
    details.appendChild(filename);

    // Create a hyperlink to the document's relative file path.
    // Using relative paths allows documents to open directly
    // from the USB without an internet connection.
    const link = document.createElement("a");
    link.href = item.path;
    link.textContent = "Open";

    // Provide an accessible description of the link for screen readers.
    link.setAttribute(
        "aria-label",
        `Open ${item.name || item.fileName}`
    );

    // Assemble the complete document entry and return it
    // to the calling render function.
    container.appendChild(details);
    container.appendChild(link);

    return container;
}

// Re-render the document list whenever the user changes the search text.
// The input event fires when text is typed, deleted or pasted.
search.addEventListener("input", event => {
    renderDocuments(event.target.value);
});

// Perform the initial render when the page loads.
// With no filter supplied, all documents in the package are displayed.
renderDocuments();
