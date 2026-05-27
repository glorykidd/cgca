export const SCHOOL_INFO = {
  name: "Cedar Grove Christian Academy",
  abbreviation: "CGCA",
  location: "Bullitt County, Kentucky",
  ministry: "Cedar Grove Baptist Church",

  mission:
    "Cedar Grove Christian Academy exists to faithfully proclaim the gospel of Jesus Christ to a lost and dying world, to equip and train students to think and live Biblically, and to prepare the next generation of Christian leaders.",
  vision:
    "Our goal is the development of students prepared academically and spiritually to serve Jesus Christ and to impact the world through their character and leadership.",
  coreValues: {
    a: "Academic Rigor",
    b: "Biblical World-view",
    c: "Christ centered",
  },
  scriptureEmphasis: "Acts 1:8",
  nonDiscrimination:
    "CGCA admits students of any race, color, and national or ethnic origin to all rights, privileges, programs, and activities.",

  programs: {
    currentGrades: ["K4/K5 (ages 4–5)"],
    enrollmentStatus: "Registration is now open for the 2026/27 school year.",
    openingsNote: "Limited openings are available — families should register soon.",
    beforeAfterCare: false,
  },

  schedule: {
    days: "Tuesday, Wednesday, and Thursday",
    hours: "9:30 AM – 12:30 PM",
    doorsOpen: "9:15 AM",
    calendarFollows: "Bullitt County Public Schools (BCPS)",
  },

  tuition: {
    schoolYear: "2026/27",
    monthly: 175,
    registrationFee: 75,
    artFee: 50,
    get totalRegistration() {
      return this.registrationFee + this.artFee;
    },
    payInFullDiscountPercent: 10,
    siblingDiscountMonthly: 10,
  },

  contact: {
    phone: "(502) 543-4101",
    email: "contactus@cedargrovechristianacademy.org",
    website: "cedargrovechristianacademy.org",
    facebook: "facebook.com/cedargrovechristianacademy",
    instagram: "instagram.com/cedargrove.christianacademy",
    linkedin: "linkedin.com/company/cg-christian-academy",
  },

  pages: {
    home: {
      url: "cedargrovechristianacademy.org",
      description: "overview, mission, core values",
    },
    about: {
      url: "cedargrovechristianacademy.org/about",
      description: "vision, philosophy, statement of faith, code of conduct, donations, swag",
    },
    parents: {
      url: "cedargrovechristianacademy.org/parents",
      description: "tuition details, school calendar, parent portal",
    },
    calendar: {
      url: "cedargrovechristianacademy.org/calendar",
      description: "embedded Google Calendar with school events",
    },
    contact: {
      url: "cedargrovechristianacademy.org/contact",
      description: "contact form, email newsletter signup",
    },
  },

  registration: {
    platform: "Praxis School",
    url: "app.praxischool.com",
    parentPortalUrl: "app.praxischool.com",
    donationsNote: "Donations accepted through Praxis School (linked on the About page).",
    swagNote: "CGCA swag/merchandise can be ordered through a form linked on the About page.",
    newsletterNote: "Families can subscribe to the email newsletter via the Contact page.",
  },

  sponsorship: {
    pageUrl: "cedargrovechristianacademy.org/sponsors",
    purpose:
      "Local businesses and individuals can partner with CGCA to support Christ-centered education in Bullitt County.",
    tiers: [
      {
        name: "Gold",
        amount: 1500,
        benefits: [
          "Mentions on the website and social media",
          "Logo placement on the school website",
          "Name on the Wall of Sponsors",
          "Larger featured logo placement on the website",
          "Signage on the playground fence",
        ],
      },
      {
        name: "Silver",
        amount: 750,
        benefits: [
          "Mentions on the website and social media",
          "Logo placement on the school website",
          "Name on the Wall of Sponsors",
        ],
      },
      {
        name: "Bronze",
        amount: 250,
        benefits: ["Mentions on the website and social media"],
      },
    ],
    howToApply:
      "Interested sponsors can fill out the inquiry form on the Sponsors page. The school will follow up by email.",
  },
};
