const API_BASE = "http://localhost:5000";

window.onload = () => {
    loadBookings();
    loadCustomers();
        
    const searchInput = document.getElementById("searchInput");
    if (searchInput) {
        searchInput.addEventListener("keyup", function (event) {
            if (event.key === "Enter") {
                searchBookings();
            }
        });
    }
};

function loadBookings(searchTerm = "") {
    fetch(`${API_BASE}/api/bookings`)
        .then(response => response.json())
        .then(bookings => {
            let filtered = bookings;

            if (searchTerm) {
                const lowerTerm = searchTerm.toLowerCase();
                filtered = bookings.filter(b =>
                    (b.resource && b.resource.toLowerCase().includes(lowerTerm)) ||
                    b.bookingID.toString().includes(lowerTerm) ||
                    b.customerID.toString().includes(lowerTerm)
                );
            }

            const list = document.getElementById('bookingList');
            list.innerHTML = '';

            const calendarEl = document.getElementById('calendar');
            calendarEl.innerHTML = '';

            if (filtered.length === 0) {
                list.innerHTML = `<div class="alert alert-warning">Ingen bookinger fundet. Prøv et andet søgeord.</div>`;
                return;
            }

            const calendar = new FullCalendar.Calendar(calendarEl, {
                initialView: 'dayGridMonth',
                height: 500,
                themeSystem: 'bootstrap5',
                events: filtered.map(b => ({
                    title: b.resource,
                    start: b.date,
                    allDay: true,
                    description: `Booking ID: ${b.bookingID}, Kunde ID: ${b.customerID}`
                })),
                eventClick: function (info) {
                    alert(info.event.title + "\n" + info.event.extendedProps.description);
                }
            });

            calendar.render();

            filtered.forEach(booking => {
                const card = document.createElement("div");
                card.className = "card booking-card col-md-4 m-2";

                card.innerHTML = `
                    <div class="card-body">
                        <h5 class="card-title">${booking.resource}</h5>
                        <p class="card-text">
                            Dato: ${new Date(booking.date).toLocaleDateString()}<br>
                            Kunde ID: ${booking.customerID}<br>
                            Booking ID: ${booking.bookingID}
                        </p>
                        <button class="btn btn-danger" onclick="deleteBooking(${booking.bookingID})">Slet</button>
                    </div>
                `;

                list.appendChild(card);
            });
        })
        .catch(error => {
            console.error("Fejl ved hentning af bookinger:", error);
            document.getElementById('bookingList').innerHTML = `<div class="alert alert-danger">Fejl under hentning af bookinger.</div>`;
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

    await fetch(`${API_BASE}/api/customers`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(customer)
    });

    await loadCustomers();
    this.reset();
    alert("Kunde oprettet!");
});

async function loadCustomers() {
    try {
        const response = await fetch(`${API_BASE}/api/customers`);
        const customers = await response.json();

        const select = document.getElementById("customerIdInput") || document.getElementById("customerID");
        if (!select) return;

        select.innerHTML = '<option value="">-- Vælg kunde --</option>';

        customers.forEach(customer => {
            const option = document.createElement("option");
            option.value = customer.customerID;
            option.textContent = `${customer.firstName} ${customer.lastName}`;
            select.appendChild(option);
        });
    } catch (err) {
        console.error("Fejl ved hentning af kunder:", err);
    }
}

function searchBookings() {
    const searchTerm = document.getElementById('searchInput').value.trim();
    loadBookings(searchTerm);
}
