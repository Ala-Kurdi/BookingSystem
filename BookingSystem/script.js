const API_BASE = "http://localhost:5000";

window.onload = loadBookings;

function loadBookings() {
    fetch(`${API_BASE}/api/bookings`)
        .then(response => response.json())
        .then(data => renderBookings(data))
        .catch(error => console.error("Fejl ved hentning af bookinger:", error));
}

function renderBookings(bookings) {
    const list = document.getElementById("bookingList");
    list.innerHTML = "";

    bookings.forEach(booking => {
        const col = document.createElement("div");
        col.className = "col";

        col.innerHTML = `
            <div class="card shadow-sm">
                <div class="card-body">
                    <h5 class="card-title">${booking.resource}</h5>
                    <p class="card-text">Dato: ${new Date(booking.date).toLocaleDateString()}</p>
                    <p class="card-text">Kunde ID: ${booking.customerID}</p>
                    <button class="btn btn-danger btn-sm" onclick="deleteBooking(${booking.bookingID})">Slet</button>
                </div>
            </div>
        `;

        list.appendChild(col);
    });
}

function createBooking() {
    const resource = document.getElementById("resourceInput").value;
    const date = document.getElementById("dateInput").value;
    const customerId = parseInt(document.getElementById("customerIdInput").value);

    if (!resource || !date || isNaN(customerId)) {
        alert("Udfyld alle felter korrekt.");
        return;
    }

    const newBooking = {
        bookingID: Math.floor(Math.random() * 10000),
        resource,
        date,
        customerID: customerId
    };

    fetch(`${API_BASE}/create`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(newBooking)
    })
        .then(res => res.text())
        .then(() => {
            loadBookings();
            document.getElementById("resourceInput").value = "";
            document.getElementById("dateInput").value = "";
            document.getElementById("customerIdInput").value = "";
        })
        .catch(err => console.error("Fejl ved oprettelse:", err));
}

function deleteBooking(id) {
    fetch(`${API_BASE}/delete/${id}`, { method: "DELETE" })
        .then(res => res.text())
        .then(() => loadBookings())
        .catch(err => console.error("Fejl ved sletning:", err));
}

document.getElementById('customerForm').addEventListener('submit', async function (e) {
    e.preventDefault();

    const customer = {
        firstName: document.getElementById('firstName').value,
        lastName: document.getElementById('lastName').value,
        email: document.getElementById('email').value
    };

    await fetch('http://localhost:5000/api/customers', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(customer)
    });

    // Opdater dropdown med ny kunde
    const select = document.getElementById('customerID');
    select.innerHTML = '<option value="">-- Vælg kunde --</option>';
    await loadCustomers();

    // Ryd formular
    this.reset();
});
